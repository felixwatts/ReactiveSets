using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Linq;

namespace ReactiveSets
{   
    public class Set<TId> : Set<TId, TId>{}

    public class Set<TId, TPayload> : ISet<TId, TPayload>, IObserver<IDelta<TId, TPayload>>, IReadOnlyDictionary<TId, TPayload>
    {
        private readonly ReferenceCounter _activator;
        private readonly ISubject<IDelta<TId, TPayload>> _subscribers;
        private Dictionary<TId, ISubject<IDelta<TId, TPayload>>> _subscribersById;
        private readonly Dictionary<TId, TPayload> _content;
        private readonly bool _disposeItemsOnDelete;
        private uint _bulkUpdateNestDepth;        

        public Set(Func<IDisposable> subscribeToSource = null, bool disposeItemsOnDelete = false)
        {
            _disposeItemsOnDelete = disposeItemsOnDelete;
            _content = new Dictionary<TId, TPayload>();
            _activator = subscribeToSource == null
                ? null
                : new ReferenceCounter(() =>
                {
                    var sub = subscribeToSource();
                    return Disposable.Create(() =>
                    {
                        sub.Dispose();
                        _content.Clear();
                        _bulkUpdateNestDepth = 0;
                    });
                });
            _subscribers = new FastSubject<IDelta<TId, TPayload>>(_activator);
        }

        public TPayload this[TId id]
        {
            get{ return _content[id]; }
        }

        public void SetItem(TId id, TPayload payload)
        {        
            var toDispose = GetAsDisposable(id);

            _content[id] = payload;
            var delta = Delta<TId, TPayload>.SetItem(id, payload);
            Publish(delta);

            toDispose?.Dispose();
        }

        public void DeleteItem(TId id)
        {
            if(!_content.ContainsKey(id)) throw new InvalidOperationException("Cannot remove non-existent item");

            var toDispose = GetAsDisposable(id);

            _content.Remove(id);
            var delta = Delta<TId, TPayload>.DeleteItem(id);
            Publish(delta);

            toDispose?.Dispose();
        }

        public void BeginBulkUpdate()
        {
            _bulkUpdateNestDepth++;
            Publish(Delta<TId, TPayload>.BeginBulkUpdate);
        }

        public void EndBulkUpdate()
        {
            if(_bulkUpdateNestDepth == 0) throw new InvalidOperationException("Bulk update nest depth < 0");

            _bulkUpdateNestDepth--;
            Publish(Delta<TId, TPayload>.EndBulkUpdate);
        }

        public void Clear()
        {
            var toDispose = GetAllAsDisposable();
            _content.Clear();
            Publish(Delta<TId, TPayload>.Clear);
            toDispose?.DisposeAll();
        }        

        public void OnCompleted()
        {
            _subscribers.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _subscribers.OnError(error);
        }

        public void OnNext(IDelta<TId, TPayload> value)
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

        public IDisposable Subscribe(TId id, IObserver<IDelta<TId, TPayload>> subscriber)
        {
            if(_subscribersById == null)
            {
                _subscribersById = new Dictionary<TId, ISubject<IDelta<TId, TPayload>>>();
            }

            if(!_subscribersById.TryGetValue(id, out var subject))
            {
                subject = new FastSubject<IDelta<TId, TPayload>>(_activator);
                _subscribersById[id] = subject;
            }

            subscriber.OnNext(Delta<TId, TPayload>.BeginBulkUpdate); 
            SendCurrentContentToSubscriber(id, subscriber);                                      
            var sub = subject.Subscribe(subscriber);                    
            subscriber.OnNext(Delta<TId, TPayload>.EndBulkUpdate);
            return sub;
        }

        public IDisposable Subscribe(IObserver<IDelta<TId, TPayload>> observer)
        {
            observer.OnNext(Delta<TId, TPayload>.BeginBulkUpdate); 
            SendCurrentContentToSubscriber(observer);                                      
            var sub = _subscribers.Subscribe(observer);                    
            observer.OnNext(Delta<TId, TPayload>.EndBulkUpdate);
            return sub;
        }

        public IEnumerable<TId> Ids => _content.Keys;
        public IEnumerable<TPayload> Payloads => _content.Values;

        public IEnumerable<TId> Keys => ((IReadOnlyDictionary<TId, TPayload>)_content).Keys;

        public IEnumerable<TPayload> Values => ((IReadOnlyDictionary<TId, TPayload>)_content).Values;

        public int Count => _content.Count;

        private void SendCurrentContentToSubscriber(IObserver<IDelta<TId, TPayload>> observer)
        {
            for(int n = 0; n < _bulkUpdateNestDepth; n++)
                observer.OnNext(Delta<TId, TPayload>.BeginBulkUpdate);

            foreach(var kvp in _content)
            {
                var delta = Delta<TId, TPayload>.SetItem(kvp.Key, kvp.Value);
                observer.OnNext(delta);
            }
        }

        private void SendCurrentContentToSubscriber(TId id, IObserver<IDelta<TId, TPayload>> observer)
        {
            for(int n = 0; n < _bulkUpdateNestDepth; n++)
                observer.OnNext(Delta<TId, TPayload>.BeginBulkUpdate);

            if(_content.TryGetValue(id, out var val))
            {
                var delta = Delta<TId, TPayload>.SetItem(id, val);
                observer.OnNext(delta);
            }
        }

        public bool ContainsKey(TId key)
        {
            return _content.ContainsKey(key);
        }

        public bool TryGetValue(TId key, out TPayload value)
        {
            return _content.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<TId, TPayload>> GetEnumerator()
        {
            return ((IReadOnlyDictionary<TId, TPayload>)_content).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IReadOnlyDictionary<TId, TPayload>)_content).GetEnumerator();
        }

        private IDisposable GetAsDisposable(TId id)
        {
            if(!_disposeItemsOnDelete)
            {
                return null;
            }

            TPayload payload;
            _content.TryGetValue(id, out payload);
            
            return payload as IDisposable;
        }

        private IEnumerable<IDisposable> GetAllAsDisposable()
        {
            if(!_disposeItemsOnDelete)
            {
                return null;
            }

            return _content.Values.OfType<IDisposable>().ToArray();
        } 

        private void Publish(IDelta<TId, TPayload> delta)
        {
            _subscribers.OnNext(delta);

            if(_subscribersById == null) return;

            switch(delta.Type)
            {
                case DeltaType.SetItem:
                case DeltaType.DeleteItem:
                {
                    if(_subscribersById.TryGetValue(delta.Id, out var subject))
                    {
                        subject.OnNext(delta);
                    }
                    break;
                }
                default:
                {
                    foreach(var subject in _subscribersById.Values)
                    {
                        subject.OnNext(delta);
                    }
                    break;
                }
            }
        }
    }
}