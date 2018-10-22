using System;
using System.Reactive.Linq;
using NUnit.Framework;
using ReactiveSets;
using Moq;
using System.Linq;
using System.Diagnostics;

namespace ReactiveSets.Test
{
    [TestFixture]
    public class FastSubject
    {
        private ReactiveSets.FastSubject<int> _subject;

        [SetUp]
        public void SetUp()
        {
            _subject = new ReactiveSets.FastSubject<int>();
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(100)]
        public void AllSubscribersReceiveUpdates(int numSubscribers)
        {
            var isCalledBySubscriber = new bool[numSubscribers];

            for(int n = 0; n < numSubscribers; n++)
            {
                var m = n;
                _subject.Subscribe(_ => isCalledBySubscriber[m] = true);
            }

            _subject.OnNext(42);            

            Assert.IsTrue(isCalledBySubscriber.All(isCalled => isCalled));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(100)]
        public void UnsubscribedSubscribersNoLongerReceiveUpdates(int numSubscribers)
        {
            int x = 0;
            var subscriptions = Enumerable.Range(0, numSubscribers).Select(_ => _subject.Subscribe(i => x = i)).ToArray();

            _subject.OnNext(42);

            subscriptions.DisposeAll();

            _subject.OnNext(43);
            Assert.AreEqual(42, x);
        }
    }
}