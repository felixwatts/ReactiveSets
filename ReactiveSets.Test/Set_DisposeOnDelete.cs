using System;
using NUnit.Framework;
using Moq;
using System.Reactive.Disposables;

namespace ReactiveSets.Test
{
    [TestFixture]
    public class Set_DisposeOnDelete
    {
        private ReactiveSets.Set<string, DisposablePayload> _subject;

        [SetUp]
        public void SetUp()
        {
            DisposablePayload.NumActivePayloads = 0;
            _subject = new Set<string, DisposablePayload>(disposeItemsOnDelete: true);
        }

        [Test]
        public void AddItem()
        {
            _subject.SetItem("A", new DisposablePayload());
            _subject.SetItem("B", new DisposablePayload());

            Assert.AreEqual(2, DisposablePayload.NumActivePayloads);
        }

        [Test]
        public void ReplaceItem()
        {
            _subject.SetItem("A", new DisposablePayload());
            _subject.SetItem("B", new DisposablePayload());

            _subject.SetItem("A", new DisposablePayload());

            Assert.AreEqual(2, DisposablePayload.NumActivePayloads);
        }

        [Test]
        public void DeleteItem()
        {
            _subject.SetItem("A", new DisposablePayload());
            _subject.SetItem("B", new DisposablePayload());

            _subject.DeleteItem("A");

            Assert.AreEqual(1, DisposablePayload.NumActivePayloads);
        }

        [Test]
        public void Clear()
        {
            _subject.SetItem("A", new DisposablePayload());
            _subject.SetItem("B", new DisposablePayload());

            _subject.Clear();

            Assert.AreEqual(0, DisposablePayload.NumActivePayloads);
        }

        [Test]
        public void Deactivate()
        {
            _subject = new Set<string, DisposablePayload>(() => Disposable.Empty, disposeItemsOnDelete: true);
            var subscription = _subject.Subscribe(new Mock<IObserver<IDelta<string, DisposablePayload>>>().Object);

            _subject.SetItem("A", new DisposablePayload());
            _subject.SetItem("B", new DisposablePayload());

            subscription.Dispose();

            Assert.AreEqual(0, DisposablePayload.NumActivePayloads);
        }

        public class DisposablePayload : IDisposable
        {
            public static int NumActivePayloads;

            public DisposablePayload()
            {
                NumActivePayloads++;
            }

            public void Dispose()
            {
                NumActivePayloads--;
            }
        }
    }
}
