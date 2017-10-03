using System;

public interface IValue<T> : IObservable<T>
{
    T Current { get; }
}