using System;
using ReactiveSets;

public class Value<T> : FastSubject<T>, IValue<T>
{
    public T Current { get; protected set; }

    public Value(T initialValue, Func<IDisposable> activate = null) : base(activate)
    {
        Current = initialValue;
    }

    public override IDisposable Subscribe(IObserver<T> observer)
    {        
        observer.OnNext(Current);
        var sub = base.Subscribe(observer);        
        return sub;
    }

    public override void OnNext(T value)
    {
        Current = value;
        base.OnNext(value);
    }
}