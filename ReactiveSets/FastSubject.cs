using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using ReactiveSets;

namespace ReactiveSets
{    
    public class FastSubject<T> : ISubject<T>, IDisposable
    {
        private IObserver<T> _singleSubscriber;
        private HashSet<IObserver<T>> _subscribersMap;
        private IObserver<T>[] _publishTo;
        private readonly ReferenceCounter _activator;

        public FastSubject(ReferenceCounter activator)
        {
            _activator = activator;
        }

        public FastSubject(Func<IDisposable> activate = null)
        {
            if(activate != null)
            {
                _activator = new ReferenceCounter(activate);
            }
        }

        public void OnCompleted()
        {
            ThrowIfNotActive();

            foreach(var s in GetSubscribers())
            {
                s.OnCompleted();
            }
        }


        public void OnError(Exception error)
        {
            ThrowIfNotActive();

            foreach(var s in GetSubscribers())
            {
                s.OnError(error);
            }
        }

        public virtual void OnNext(T value)
        {
            ThrowIfNotActive();
            
            foreach(var s in GetSubscribers())
            {
                s.OnNext(value);
            }
        }        

        public virtual IDisposable Subscribe(IObserver<T> observer)
        {            
            if(observer == null) throw new ArgumentNullException("observer");

            _publishTo = null;

            if(_subscribersMap != null)
            {
                _subscribersMap.Add(observer);
            }
            else if(_singleSubscriber == null)
            {
                _singleSubscriber = observer;
            }
            else
            {
                _subscribersMap = new HashSet<IObserver<T>>();
                _subscribersMap.Add(_singleSubscriber);
                _singleSubscriber = null;
                _subscribersMap.Add(observer);
            }

            _activator?.IncrementReferenceCount();

            return Disposable.Create(() => Unsubscribe(observer));
        }

        public virtual void Dispose()
        {
            _publishTo = null;
            _singleSubscriber = null;
            _subscribersMap = null;
        }

        private void Unsubscribe(IObserver<T> observer)
        {
            _singleSubscriber = null;
            _subscribersMap?.Remove(observer);
            _publishTo = null;
            _activator?.DecrementReferenceCount();
        }

        private IObserver<T>[] GetSubscribers()
        {
            if(_publishTo == null)
            {
                if(_singleSubscriber != null)
                {
                    _publishTo = new[]{ _singleSubscriber };
                }
                else if(_subscribersMap != null)
                {
                    _publishTo = _subscribersMap.ToArray();
                }                
                else _publishTo = new IObserver<T>[0];
            }

            return _publishTo;
        }

        private void ThrowIfNotActive()
        {
            if(_activator == null) return;
            if(!_activator.IsActive) throw new InvalidOperationException("When an activator is used it is invalid to publish when there are no subscribers");
        }
    }
}