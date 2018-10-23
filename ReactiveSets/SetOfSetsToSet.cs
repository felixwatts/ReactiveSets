using System;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace ReactiveSets
{    
    public abstract class SetOfSetsToSet<TIdSet, TId, TPayloadIn, TPayloadOut> : ISet<TId, TPayloadOut>, IObserver<IDelta<TIdSet, IObservable<IDelta<TId, TPayloadIn>>>>
    {
        protected readonly Set<TId, TPayloadOut> _content;
        private readonly IObservable<IDelta<TIdSet, IObservable<IDelta<TId, TPayloadIn>>>> _source;

        private Dictionary<TIdSet, IDisposable> _setSubscriptionBySetId;

        protected SetOfSetsToSet(IObservable<IDelta<TIdSet, IObservable<IDelta<TId, TPayloadIn>>>> source)
        {
            _setSubscriptionBySetId = new Dictionary<TIdSet, IDisposable>();
            _content = new Set<TId, TPayloadOut>(SubscribeToSource);
            _source = source;
        }

        public virtual void OnCompleted()
        {
            _content.OnCompleted();
        }

        public virtual void OnError(Exception exception)
        {
            _content.OnError(exception);
        }

        public virtual void OnNext(IDelta<TIdSet, IObservable<IDelta<TId, TPayloadIn>>> next)
        {
            switch(next.Type)
            {
                case DeltaType.BeginBulkUpdate:
                    OnBeginBulkUpdateSets();
                    break;
                case DeltaType.EndBulkUpdate:
                    OnEndBulkUpdateSets();
                    break;
                case DeltaType.Clear:
                    OnClearSets();
                    break;
                case DeltaType.SetItem:
                    OnSetSet(next.Id, next.Payload);
                    break;
                case DeltaType.DeleteItem:
                    OnDeleteSet(next.Id);
                    break;
            }
        }

        public IDisposable Subscribe(IObserver<IDelta<TId, TPayloadOut>> observer)
        {
            return _content.Subscribe(observer);
        }

        private IDisposable SubscribeToSource()
        {
            return _source.Subscribe(this);
        }

        protected virtual void OnBeginBulkUpdateSets()
        {
            _content.BeginBulkUpdate();
        }

        protected virtual void OnEndBulkUpdateSets()
        {
            _content.EndBulkUpdate();
        }

        protected virtual void OnClearSets()
        {
            _content.Clear();
            foreach(var sub in _setSubscriptionBySetId.Values)
            {
                sub.Dispose();
            }
            _setSubscriptionBySetId.Clear();
            _content.Clear();
        }

        protected virtual void OnError(TIdSet setId, Exception error)
        {
            _content.OnError(error);
        }

        protected virtual void OnCompleted(TIdSet setId)
        {
            _content.OnCompleted();
        }

        protected virtual void OnNext(TIdSet setId, IDelta<TId, TPayloadIn> next)
        {
            switch(next.Type)
            {
                case DeltaType.BeginBulkUpdate:
                    OnBeginBulkUpdate(setId);
                    break;
                case DeltaType.EndBulkUpdate:
                    OnEndBulkUpdate(setId);
                    break;
                case DeltaType.Clear:
                    OnClear(setId);
                    break;
                case DeltaType.SetItem:
                    OnSetItem(setId, next.Id, next.Payload);
                    break;
                case DeltaType.DeleteItem:
                    OnDeleteItem(setId, next.Id);
                    break;
            }
        }

        protected abstract void OnSetItem(TIdSet setId, TId id, TPayloadIn payload);
        protected abstract void OnDeleteItem(TIdSet setId, TId id);
        protected abstract void OnClear(TIdSet setId);

        protected virtual void OnBeginBulkUpdate(TIdSet setId)
        {            
            _content.BeginBulkUpdate();
        }

        protected virtual void OnEndBulkUpdate(TIdSet setId)
        {
            _content.EndBulkUpdate();
        }

        protected virtual void OnSetSet(TIdSet id, IObservable<IDelta<TId, TPayloadIn>> payload)
        {
            IDisposable oldSubscription = null;
            _setSubscriptionBySetId.TryGetValue(id, out oldSubscription);
            oldSubscription?.Dispose();

            var newSubscription = payload.Subscribe(next => OnNext(id, next), error => OnError(id, error), () => OnCompleted(id));
        }

        protected virtual void OnDeleteSet(TIdSet id)
        {
            _setSubscriptionBySetId[id].Dispose();
            _setSubscriptionBySetId.Remove(id);
        }
    }    
}