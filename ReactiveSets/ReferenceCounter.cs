using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;

namespace ReactiveSets
{
    public class ReferenceCounter : IDisposable
    {
        private readonly Func<IDisposable> _activate;
        private uint _referenceCount;        
        private IDisposable _subscription;

        public ReferenceCounter(Func<IDisposable> activate)
        {
            _activate = activate;
        }

        public bool IsActive => _referenceCount > 0;

        public void IncrementReferenceCount()
        {
            _referenceCount++;

            if(_referenceCount == 1)
            {
                _subscription = _activate();
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