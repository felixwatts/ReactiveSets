using System;
using System.Reactive.Disposables;

namespace ReactiveSets
{
    public abstract class TwoSetsToSet<TIdLeft, TPayloadLeft, TIdRight, TPayloadRight, TIdOut, TPayloadOut> 
        : ISet<TIdOut, TPayloadOut>
    {
        protected Set<TIdOut, TPayloadOut> _content;
        private ISet<TIdLeft, TPayloadLeft> _left;
        private ISet<TIdRight, TPayloadRight> _right;

        protected TwoSetsToSet(
            ISet<TIdLeft, TPayloadLeft> left, 
            ISet<TIdRight, TPayloadRight> right,
            bool disposeItemsOnDelete)
        {
            _left = left;
            _right = right;
            _content = new Set<TIdOut, TPayloadOut>(SubscribeToSources, disposeItemsOnDelete);
        }

        public IDisposable Subscribe(IObserver<IDelta<TIdOut, TPayloadOut>> observer)
        {
            return _content.Subscribe(observer);
        }

        private IDisposable SubscribeToSources()
        {
            var subLeft = _left.Subscribe(OnNextLeft, OnError, OnCompleted);
            var subRight = _right.Subscribe(OnNextRight, OnError, OnCompleted);

            return Disposable.Create(() =>
            {
                subLeft.Dispose();
                subRight.Dispose();
                Reset();
            });
        }        

        private void OnNextLeft(IDelta<TIdLeft, TPayloadLeft> next)
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
                    OnClearLeft();
                    break;
                case DeltaType.SetItem:
                    OnSetItemLeft(next.Id, next.Payload);
                    break;
                case DeltaType.DeleteItem:
                    OnDeleteItemLeft(next.Id);
                    break;
            }
        }

        private void OnNextRight(IDelta<TIdRight, TPayloadRight> next)
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
                    OnClearRight();
                    break;
                case DeltaType.SetItem:
                    OnSetItemRight(next.Id, next.Payload);
                    break;
                case DeltaType.DeleteItem:
                    OnDeleteItemRight(next.Id);
                    break;
            }
        }

        internal abstract void OnDeleteItemRight(TIdRight id);
        internal abstract void OnSetItemRight(TIdRight id, TPayloadRight payload);
        internal abstract void OnClearRight();

        internal abstract void OnDeleteItemLeft(TIdLeft id);
        internal abstract void OnSetItemLeft(TIdLeft id, TPayloadLeft payload);
        internal abstract void OnClearLeft();

        protected virtual void OnEndBulkUpdate()
        {
            _content.EndBulkUpdate();
        }

        protected virtual void OnBeginBulkUpdate()
        {
            _content.BeginBulkUpdate();
        }

        protected virtual void OnError(Exception error)
        {
            _content.OnError(error);
        }

        protected virtual void OnCompleted()
        {
            _content.OnCompleted();
        }

        protected abstract void Reset();
    }
}