using System;
using System.Linq;
using NUnit.Framework;
using Moq;
using ReactiveSets;
using System.Collections;
using System.Collections.Generic;

namespace ReactiveSets.Test
{
    [TestFixture]
    public class Snapshot
    {
        private Set<int, int> _source;

        [SetUp]
        public void SetUp()
        {
            _source = new Set<int, int>();
        }

        [TestCase(false, true)]
        [TestCase(true, false)] 
        public void CallbackIsCalledSynchonouslyOnlyIfSourceIsNotInBulkUpdate(bool isSourceInBulkUpdate, bool expectedIsCallbackCalledSynchronously) 
        {        
            if(isSourceInBulkUpdate)
            {
                _source.BeginBulkUpdate();
            }

            bool actualIsCallbackCalledSynchronously = false;

            _source.Snapshot(ss => actualIsCallbackCalledSynchronously = true);

            Assert.AreEqual(expectedIsCallbackCalledSynchronously, actualIsCallbackCalledSynchronously);     
        }  

        [TestCase(0)]
        [TestCase(10)]
        public void SnapshotHasCorrectContents(int numItemsInSource)
        {
            for(int n = 0; n < numItemsInSource; n++)
            {
                _source.SetItem(n, n);
            }

            _source.Snapshot(ss => 
            {
                Assert.AreEqual(numItemsInSource, ss.Count);
                for(int n = 0; n < numItemsInSource; n++)
                {
                    Assert.AreEqual(ss[n], n);
                }
            });
        }

        [TestCase(0)]
        [TestCase(10)]
        public void SnapshotHasCorrectContentsAfterBulkUpdateEnds(int numItemsInSource)
        {
            _source.BeginBulkUpdate();

            IReadOnlyDictionary<int, int> snapshot = null;

            _source.Snapshot(ss => snapshot = ss);
            
            for(int n = 0; n < numItemsInSource; n++)
            {
                _source.SetItem(n, n);
            }

            _source.EndBulkUpdate();

            Assert.NotNull(snapshot);
            Assert.AreEqual(numItemsInSource, snapshot.Count);
            for(int n = 0; n < numItemsInSource; n++)
            {
                Assert.AreEqual(snapshot[n], n);
            }
        }
    }
}
