using System;
using System.Collections;
using System.Collections.Generic;
using ReactiveSets;

internal class Snapshotter<TId, TPayload> : IObserver<Delta<TId, TPayload>>
{
    private readonly Dictionary<TId, TPayload> _content;
    private readonly IDisposable _subscription;
    private readonly Action<IReadOnlyDictionary<TId, TPayload>> _handleResult;
    private uint _bulkUpdateNestDepth;

    public Snapshotter(ISet<TId, TPayload> source, Action<IReadOnlyDictionary<TId, TPayload>> handleResult)
    {
        _content = new Dictionary<TId, TPayload>();
        _handleResult = handleResult;
        _subscription = source.Subscribe(this);
        if(_bulkUpdateNestDepth == 0)
        {
            _subscription.Dispose();
        }
    }

    private void Complete()
    {
        _subscription?.Dispose();
        _handleResult(_content);
    }

    public void OnCompleted()
    {
        Complete();
    }

    public void OnError(Exception error)
    {
        throw error;
    }

    public void OnNext(Delta<TId, TPayload> delta)
    {
        switch(delta.Type)
        {
            case DeltaType.BeginBulkUpdate:
                _bulkUpdateNestDepth++;
                break;
            case DeltaType.EndBulkUpdate:
                _bulkUpdateNestDepth--;
                if(_bulkUpdateNestDepth == 0)
                {
                    Complete();
                }
                break;
            case DeltaType.SetItem:
                _content.Add(delta.Id, delta.Payload);
                break;
            case DeltaType.Clear:
            case DeltaType.DeleteItem:
                throw new InvalidOperationException();
        }
    }
}