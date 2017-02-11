using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;

namespace ReactiveSets
{
    public class ReferenceCountingSubscriber : IDisposable
    {
        private readonly Func<IDisposable> _subscribe;
        private uint _referenceCount;        
        private IDisposable _subscription;

        public ReferenceCountingSubscriber(Func<IDisposable> subscribe)
        {
            _subscribe = subscribe;
        }

        public void IncrementReferenceCount()
        {
            _referenceCount++;

            if(_referenceCount == 1)
            {
                _subscription = _subscribe();
            }
        }

        public void DecrementReferenceCount()
        {
            if(_referenceCount == 0) throw new InvalidOperationException();

            _referenceCount--;

            if(_referenceCount == 0)
            {
                _subscription.Dispose();
                _subscription = null;
            }
        }

        public void Dispose()
        {
            _referenceCount = 0;
            _subscription?.Dispose();
        }
    }
}