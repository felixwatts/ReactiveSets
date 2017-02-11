using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;

namespace ReactiveSets
{
    public class ReferenceCountingSubject<T> : FastSubject<T>
    {
        private readonly ReferenceCountingSubscriber _referenceCounter;

        public ReferenceCountingSubject(Func<IDisposable> subscribe)
        {
            _referenceCounter = new ReferenceCountingSubscriber(subscribe);            
        }

        public override IDisposable Subscribe(IObserver<T> observer)
        {
            var sub = base.Subscribe(observer);

            _referenceCounter.IncrementReferenceCount();

            return Disposable.Create(() =>
            {
                sub.Dispose();
                _referenceCounter.DecrementReferenceCount();
            });
        }

        public override void Dispose()
        {
            _referenceCounter.Dispose();
            base.Dispose();
        }
    }
}