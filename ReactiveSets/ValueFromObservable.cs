using System;
using ReactiveSets;

namespace ReactiveSets
{
    public class ValueFromObservable<T> : IObserver<T>, IValue<T>
    {
        private readonly IObservable<T> _source;
        private readonly Value<T> _content;

        public ValueFromObservable(IObservable<T> source, T initialValue)
        {
            _source = source;
            _content = new Value<T>(initialValue, SubscribeToSource);
        }

        public T Current => _content.Current;

        public void OnCompleted()
        {
            _content.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _content.OnError(error);
        }

        public void OnNext(T value)
        {
            _content.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _content.Subscribe(observer);
        }

        private IDisposable SubscribeToSource()
        {
            return _source.Subscribe(this);
        }
    }
}
