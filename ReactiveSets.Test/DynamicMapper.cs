using System;
using System.Reactive.Linq;
using NUnit.Framework;
using ReactiveSets;
using Moq;


namespace ReactiveSets.Test
{
    [TestFixture]
    public class DynamicMapper
    {
        Set<string, IValue<int>> _source;
        ISet<string, int> _subject;

        [SetUp]
        public void SetUp()
        {
            _source = new Set<string, IValue<int>>();
            _subject = _source.SelectDynamic(s => s.Select(x => x * 2).ToValue(0));
        }

        [Test]
        public void SubscribeCycle()
        {
            _source.SetItem("A", new Value<int>(1));
            var sub = _subject.Subscribe();
            sub.Dispose();
            sub = _subject.Subscribe();
        }

        [Test]
        public void InitialValueIsMapped()
        {
            _source.SetItem("A", new Value<int>(1));
            _source.SetItem("B", new Value<int>(2));

            var hasProcessedSnapshot = false;
            _subject.Snapshot(ss => 
            {
                hasProcessedSnapshot = true;
                Assert.AreEqual(2, ss.Count);
                Assert.AreEqual(2, ss["A"]);
                Assert.AreEqual(4, ss["B"]);
            });

            Assert.IsTrue(hasProcessedSnapshot);
        }

        [Test]
        public void ChangedValueIsMapped()
        {
            _source.SetItem("A", new Value<int>(1));
            _source.SetItem("A", new Value<int>(3));

            var hasProcessedSnapshot = false;
            _subject.Snapshot(ss => 
            {
                hasProcessedSnapshot = true;
                Assert.AreEqual(1, ss.Count);
                Assert.AreEqual(6, ss["A"]);
            });

            Assert.IsTrue(hasProcessedSnapshot);
        }

        [Test]
        public void RemovedItemIsRemoved()
        {
            _subject.Subscribe();

            _source.SetItem("A", new Value<int>(1));
            _source.DeleteItem("A");

            var hasProcessedSnapshot = false;
            _subject.Snapshot(ss => 
            {
                hasProcessedSnapshot = true;
                Assert.AreEqual(0, ss.Count);
            });

            Assert.IsTrue(hasProcessedSnapshot);
        }

        [Test]
        public void KeyIsUnsubscribedWhenRemoved()
        {
            _subject.Subscribe();

            var key = new Mock<IValue<int>>();
            var sub = new Mock<IDisposable>();
            key.Setup(k => k.Subscribe(It.IsAny<IObserver<int>>())).Returns(sub.Object);

            _source.SetItem("A", key.Object);

            _source.DeleteItem("A");

            sub.Verify(s => s.Dispose(), Times.Once());
        }

        [Test]
        public void KeyIsUnsubscribedWhenModified()
        {
            _subject.Subscribe();

            var key = new Mock<IValue<int>>();
            var sub = new Mock<IDisposable>();
            key.Setup(k => k.Subscribe(It.IsAny<IObserver<int>>())).Returns(sub.Object);

            _source.SetItem("A", key.Object);

            var key2 = new Mock<IValue<int>>();
            var sub2 = new Mock<IDisposable>();
            key2.Setup(k => k.Subscribe(It.IsAny<IObserver<int>>())).Returns(sub2.Object);
            _source.SetItem("A", key2.Object);

            sub.Verify(s => s.Dispose(), Times.Once());
            sub2.Verify(s => s.Dispose(), Times.Never());
        }
    }
}

