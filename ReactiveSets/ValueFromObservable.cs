using System;

public class ValueFromObservable<T> : Value<T>, IObserver<T>
{
    private IDisposable _subscription;

    public ValueFromObservable(IObservable<T> source, T initialValue) : base(initialValue)
    {
        _subscription = source.Subscribe(this);
    }
    public override void Dispose()
    {
        _subscription.Dispose();
        base.Dispose();
    }
}