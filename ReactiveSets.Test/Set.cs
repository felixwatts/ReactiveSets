﻿using System;
using System.Linq;
using NUnit.Framework;
using Moq;

namespace ReactiveSets.Test
{
    [TestFixture]
    public class Set
    {
        private ReactiveSets.Set<string, int> _subject;
        private Mock<IObserver<Delta<string, int>>> _subscriber;
        private IDisposable _subscription;

        [Test]
        public void IsInitiallyEmpty() 
        {
            GivenANewSet();

            Assert.IsEmpty(_subject.Ids);
            Assert.IsEmpty(_subject.Payloads);
            Assert.AreEqual(0, _subject.Count());
        }  

        [Test]
        public void SubscribeEmptySet()
        {
            GivenANewSet();
            GivenASubscriber();

            ThenTheSubscriberReceivesNumMessages(2);
            ThenTheSubscriberReceives(DeltaType.BeginBulkUpdate, 1);
            ThenTheSubscriberReceives(DeltaType.EndBulkUpdate, 1);
        }

        [Test]
        public void AddItemNoSubscriber()
        {
            GivenANewSet();

            WhenAnItemIsSet("miaow", 42);

            ThenTheIdsPropertyContains("miaow");
            ThenThePayloadsPropertyContains(42);  
      
        } 

        [Test]
        public void AddItemWithSubscriber()
        {
            GivenANewSet();
            GivenASubscriber();

            WhenAnItemIsSet("miaow", 42);

            ThenTheIdsPropertyContains("miaow");
            ThenThePayloadsPropertyContains(42);  
            ThenTheSubscriberReceivesNumMessages(3);
            ThenTheSubscriberReceives(DeltaType.BeginBulkUpdate, 1);
            ThenTheSubscriberReceives(DeltaType.SetItem, "miaow", 42, 1);
            ThenTheSubscriberReceives(DeltaType.EndBulkUpdate, 1);
        } 

        private void ThenTheIdsPropertyContains(string id)
        {
            Assert.IsTrue(_subject.Ids.Contains(id));
        }

        private void ThenThePayloadsPropertyContains(int val)
        {
            Assert.IsTrue(_subject.Payloads.Contains(val));
        }

        private void WhenAnItemIsSet(string key, int val)
        {
            _subject.SetItem(key, val);
        }

        private void GivenANewSet()
        {
            _subject = new ReactiveSets.Set<string, int>();
        }

        private void GivenASubscriber()
        {
            _subscriber = new Mock<IObserver<Delta<string, int>>>();
            _subscription = _subject.Subscribe(_subscriber.Object);
        }

        private void ThenTheSubscriberReceivesNumMessages(int count)
        {
            _subscriber.Verify(s => s.OnNext(It.IsAny<Delta<string, int>>()), Times.Exactly(count));
        }

        private void ThenTheSubscriberReceives(DeltaType type, int times)
        {
            _subscriber.Verify(s => s.OnNext(It.Is<Delta<string, int>>(d => d.Type == type)), Times.Exactly(times));
        }

        private void ThenTheSubscriberReceives(DeltaType type, string id, int payload, int times)
        {
            _subscriber.Verify(s => s.OnNext(It.Is<Delta<string, int>>(d => d.Type == type && d.Id == id && d.Payload == payload)), Times.Exactly(times));
        }
    }
}
