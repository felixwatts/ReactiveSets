using System;
using ReactiveSets;

public class Value<T> : FastSubject<T>, IValue<T>
{
    public T Current { get; private set; }

    public Value(T initialValue)
    {
        Current = initialValue;
    }

    public override IDisposable Subscribe(IObserver<T> observer)
    {
        observer.OnNext(Current);
        return base.Subscribe(observer);
    }

    public override void OnNext(T value)
    {
        Current = value;
        base.OnNext(value);
    }
}