using System;
using System.Reactive;

namespace ReactiveSets
{
    public abstract class SetOfSetsToSet<TIdSet, TIdIn, TPayloadIn, TIdOut, TPayloadOut> : ISet<TIdOut, TPayloadOut>, IObserver<Delta<TIdSet, ISet<TIdIn, TPayloadIn>>>
    {
        protected readonly Set<TIdOut, TPayloadOut> _content;
        private readonly ISet<TIdSet, ISet<TIdIn, TPayloadIn>> _source;

        protected SetOfSetsToSet(ISet<TIdSet, ISet<TIdIn, TPayloadIn>> source)
        {
            _content = new Set<TIdOut, TPayloadOut>(SubscribeToSource);
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

        public virtual void OnNext(Delta<TIdSet, ISet<TIdIn, TPayloadIn>> next)
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

        public IDisposable Subscribe(IObserver<Delta<TIdOut, TPayloadOut>> observer)
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
        }

        protected abstract void OnSetSet(TIdSet id, ISet<TIdIn, TPayloadIn> payload);

        protected abstract void OnDeleteSet(TIdSet id);
    }

    public abstract class SetToSet<TIdIn, TPayloadIn, TIdOut, TPayloadOut> : ISet<TIdOut, TPayloadOut>, IObserver<Delta<TIdIn, TPayloadIn>>
    {
        protected readonly Set<TIdOut, TPayloadOut> _content;

        protected SetToSet(ISet<TIdIn, TPayloadIn> source)
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