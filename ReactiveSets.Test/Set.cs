using System;
using System.Linq;
using NUnit.Framework;
using Moq;
using System.Reactive.Disposables;

namespace ReactiveSets.Test
{

    [TestFixture]
    public class Set
    {
        private ReactiveSets.Set<string, int> _subject;
        private Mock<IObserver<IDelta<string, int>>> _subscriber;
        private IDisposable _subscription;
        private int _activationCount;

        [SetUp]
        public void SetUp()
        {
            _activationCount = 0;
        }

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
        public void DeleteItemNoSubscriber()
        {
            GivenANewSet();

            WhenAnItemIsSet("miaow", 42);
            WhenAnItemIsDeleted("miaow");

            ThenTheIdsPropertyDoesNotContain("miaow");
            ThenThePayloadsPropertyDoesNotContain(42);        
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

        [Test]
        public void DeleteItemWithSubscriber()
        {
            GivenANewSet();
            GivenASubscriber();

            WhenAnItemIsSet("miaow", 42);
            WhenAnItemIsDeleted("miaow");

            ThenTheIdsPropertyDoesNotContain("miaow");
            ThenThePayloadsPropertyDoesNotContain(42);  
            ThenTheSubscriberReceivesNumMessages(4);
            ThenTheSubscriberReceives(DeltaType.BeginBulkUpdate, 1);
            ThenTheSubscriberReceives(DeltaType.SetItem, "miaow", 42, 1);
            ThenTheSubscriberReceives(DeltaType.EndBulkUpdate, 1);
            ThenTheSubscriberReceives(DeltaType.DeleteItem, "miaow", 0, 1);
        } 

        [Test]
        public void ClearWithNoSubscribers()
        {
            GivenANewSet();
            GivenAnItemIsSet("A", 1);
            GivenAnItemIsSet("B", 2);

            WhenTheSetIsCleared();

            ThenTheSetIsEmpty(); 
        }

        [Test]
        public void ClearWithSubscriber()
        {
            GivenANewSet();
            GivenASubscriber();
            GivenAnItemIsSet("A", 1);
            GivenAnItemIsSet("B", 2);

            WhenTheSetIsCleared();

            ThenTheSetIsEmpty(); 
            ThenTheSubscriberReceivesNumMessages(5);
            ThenTheSubscriberReceives(DeltaType.BeginBulkUpdate, 1);
            ThenTheSubscriberReceives(DeltaType.EndBulkUpdate, 1);
            ThenTheSubscriberReceives(DeltaType.SetItem, "A", 1,  1);
            ThenTheSubscriberReceives(DeltaType.SetItem, "B", 2,  1);
            ThenTheSubscriberReceives(DeltaType.Clear, 1);
        }

        [Test]
        public void SubscribeCycle()
        {
            GivenANewSet();
            GivenASubscriber();
            GivenAnItemIsSet("A", 1);
            GivenSubscriberUnsubscribes();

            WhenASubscriberSubscribes();

            ThenTheIdsPropertyContains("A");
            ThenThePayloadsPropertyContains(1);  
            ThenTheSubscriberReceivesNumMessages(3);
            ThenTheSubscriberReceives(DeltaType.BeginBulkUpdate, 1);
            ThenTheSubscriberReceives(DeltaType.SetItem, "A", 1, 1);
            ThenTheSubscriberReceives(DeltaType.EndBulkUpdate, 1);
        }

        [Test]
        public void Activation_InitiallyNotActive()
        {
            GivenANewSet(true);
            
            ThenTheSourceIsNotActive();
        }

        [Test]
        public void Activation_Activate()
        {
            GivenANewSet(true);

            WhenASubscriberSubscribes();
            
            ThenTheSourceIsActive();
        }

        [Test]
        public void Activation_Deactivate()
        {
            GivenANewSet(true);
            GivenASubscriber();

            WhenSubscriberUnsubscribes();
            
            ThenTheSourceIsNotActive();
        }

        [Test]
        public void Activation_Reactivate()
        {
            GivenANewSet(true);
            GivenASubscriber();
            GivenSubscriberUnsubscribes();

            WhenASubscriberSubscribes();
            
            ThenTheSourceIsActive();
        }

        [Test]
        public void Activation_SubscribeToItem_Activate()
        {
            GivenANewSet(true);

            WhenSubscriberSubscribesToItem("A");
            
            ThenTheSourceIsActive();
        }

        [Test]
        public void Activation_SubscribeToItem_Deactivate()
        {
            GivenANewSet(true);
            GivenASubscriberToItem("A");

            WhenSubscriberUnsubscribes();
            
            ThenTheSourceIsNotActive();
        }

        [Test]
        public void Activation_SubscribeToItem_Reactivate()
        {
            GivenANewSet(true);
            GivenASubscriberToItem("A");
            GivenSubscriberUnsubscribes();

            WhenSubscriberSubscribesToItem("A");
            
            ThenTheSourceIsActive();
        }

        [Test]
        public void SubscribeToItem_ShouldReceiveCommonMessages()
        {
            GivenANewSet();
            GivenASubscriberToItem("A");

            WhenAnItemIsSet("B", 1);
            WhenAnItemIsDeleted("B");
            WhenTheSetIsCleared();

            ThenTheSubscriberReceivesNumMessages(3);
            ThenTheSubscriberReceives(DeltaType.BeginBulkUpdate, 1);
            ThenTheSubscriberReceives(DeltaType.EndBulkUpdate, 1);
            ThenTheSubscriberReceives(DeltaType.Clear, 1);
        }

        [Test]
        public void SubscribeToItem_ShouldReceiveItemMessages()
        {
            GivenANewSet();
            GivenASubscriberToItem("A");

            WhenAnItemIsSet("A", 1);
            WhenAnItemIsDeleted("A");
            WhenTheSetIsCleared();

            ThenTheSubscriberReceivesNumMessages(5);
            ThenTheSubscriberReceives(DeltaType.BeginBulkUpdate, 1);
            ThenTheSubscriberReceives(DeltaType.EndBulkUpdate, 1);
            ThenTheSubscriberReceives(DeltaType.SetItem, "A", 1, 1);
            ThenTheSubscriberReceives(DeltaType.DeleteItem, "A", 0, 1);
            ThenTheSubscriberReceives(DeltaType.Clear, 1);
        }

        [Test]
        public void SubscribeToItem_Unsubscribe()
        {
            GivenANewSet();
            GivenASubscriberToItem("A");
            GivenSubscriberUnsubscribes();

            WhenAnItemIsSet("A", 1);
            WhenAnItemIsDeleted("A");
            WhenAnItemIsSet("B", 2);
            WhenAnItemIsDeleted("B");
            WhenTheSetIsCleared();

            ThenTheSubscriberReceivesNumMessages(2);
            ThenTheSubscriberReceives(DeltaType.BeginBulkUpdate, 1);
            ThenTheSubscriberReceives(DeltaType.EndBulkUpdate, 1);
        }

        private void GivenSubscriberUnsubscribes()
        {
            _subscription.Dispose();
        }

        private void ThenTheIdsPropertyContains(string id)
        {
            Assert.IsTrue(_subject.Ids.Contains(id));
        }

        private void ThenTheIdsPropertyDoesNotContain(string id)
        {
            Assert.IsFalse(_subject.Ids.Contains(id));
        }

        private void ThenThePayloadsPropertyContains(int val)
        {
            Assert.IsTrue(_subject.Payloads.Contains(val));
        }

        private void ThenThePayloadsPropertyDoesNotContain(int val)
        {
            Assert.IsFalse(_subject.Payloads.Contains(val));
        }

        private void GivenAnItemIsSet(string key, int val)
        {
            _subject.SetItem(key, val);
        }

        private void WhenAnItemIsSet(string key, int val)
        {
            _subject.SetItem(key, val);
        }

        private void WhenAnItemIsDeleted(string key)
        {
            _subject.DeleteItem(key);
        }

        private void GivenANewSet(bool countActivation = false)
        {            
            if(countActivation)
                _subject = new ReactiveSets.Set<string, int>(Activate);
            else
                _subject = new ReactiveSets.Set<string, int>();
        }

        private IDisposable Activate()
        {
            _activationCount ++;
            return Disposable.Create(() => _activationCount--);
        }

        private void WhenASubscriberSubscribes()
        {
            GivenASubscriber();
        }

        private void WhenSubscriberSubscribesToItem(string id)
        {
            GivenASubscriberToItem(id);
        }

        private void WhenSubscriberUnsubscribes()
        {
            GivenSubscriberUnsubscribes();
        }

        private void WhenTheSetIsCleared()
        {
            _subject.Clear();
        }

        private void GivenASubscriber()
        {
            _subscriber = new Mock<IObserver<IDelta<string, int>>>();
            _subscription = _subject.Subscribe(_subscriber.Object);
        }

        private void GivenASubscriberToItem(string id)
        {
            _subscriber = new Mock<IObserver<IDelta<string, int>>>();
            _subscription = _subject.Subscribe(id, _subscriber.Object);
        }

        private void ThenTheSubscriberReceivesNumMessages(int count)
        {
            _subscriber.Verify(s => s.OnNext(It.IsAny<IDelta<string, int>>()), Times.Exactly(count));
        }

        private void ThenTheSubscriberReceives(DeltaType type, int times)
        {
            _subscriber.Verify(s => s.OnNext(It.Is<IDelta<string, int>>(d => d.Type == type)), Times.Exactly(times));
        }

        private void ThenTheSubscriberReceives(DeltaType type, string id, int payload, int times)
        {
            _subscriber.Verify(s => s.OnNext(It.Is<IDelta<string, int>>(d => d.Type == type && d.Id == id && d.Payload == payload)), Times.Exactly(times));
        }

        private void ThenTheSetIsEmpty()
        {
            Assert.IsEmpty(_subject.Ids);
            Assert.IsEmpty(_subject.Values);
            Assert.AreEqual(0, _subject.Count);
        }

        private void ThenTheSourceIsNotActive()
        {
            Assert.AreEqual(0, _activationCount);
        }

        private void ThenTheSourceIsActive()
        {
            Assert.AreEqual(1, _activationCount);
        }
    }
}
