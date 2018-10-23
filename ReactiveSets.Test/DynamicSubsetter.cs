using System;
using System.Linq;
using NUnit.Framework;
using Moq;
using System.Reactive.Subjects;

namespace ReactiveSets.Test
{
    [TestFixture]
    public class DynamicSubsetter
    {
        internal class Item
        {
            public Value<int> Key { get; }
            public string Name { get; }

            public Item(string name, int initialKey)
            {
                Name = name;
                Key = new Value<int>(initialKey);
            }
        }

        private ReactiveSets.Set<string, Item> _source;

        private ReactiveSets.ISet<string, Item> _subject;
        private IDisposable _subscription;

        [SetUp]
        public void SetUp()
        {
            _source = new Set<string, Item>();
            _subject = _source.WhereDynamic((_, item) => item.Key, n => n >= 5);
            _subscription = _subject.Subscribe();
        }

        [Test]
        public void EmptyWhenSourceIsEmpty() 
        {
            ThenItemCountIs(0);
        }  

        [TestCase(1, false)]
        [TestCase(5, true)]
        public void InitialKeyValue(int initialKeyValue, bool expectedIsItemInitiallyIncluded)
        {
            var itemA = new Item("A", initialKeyValue);            
            _source.SetItem("A", itemA);

            ThenItemCountIs(expectedIsItemInitiallyIncluded ? 1 : 0);
        }

        [TestCase(1, 2, false)]
        [TestCase(1, 5, true)]
        [TestCase(5, 6, true)]
        [TestCase(5, 4, false)]
        public void ChangeOfKeyValue(int initialKeyValue, int newKeyValue, bool expectedIsItemEventuallyIncluded)
        {
            _subject.Subscribe();

            var itemA = new Item("A", initialKeyValue);            
            _source.SetItem("A", itemA);
            itemA.Key.OnNext(newKeyValue);

            ThenItemCountIs(expectedIsItemEventuallyIncluded ? 1 : 0);
        }

        [Test]
        public void SubscribeCycle()
        {
            var itemA = new Item("A", 5);            
            _source.SetItem("A", itemA);

            _subscription.Dispose();

            _subscription = _subject.Subscribe();

             ThenItemCountIs(1);
        }

        private void ThenItemCountIs(int n)
        {
            _subject.Snapshot(ss => Assert.AreEqual(n, ss.Count));           
        }
    }
}
