using System;
using System.Reactive.Linq;
using NUnit.Framework;
using ReactiveSets;
using Moq;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReactiveSets.Test
{
    [TestFixture]
    public class Sorter
    {
        Set<int> _source;
        ISet<int, int> _subject;

        [SetUp]
        public void SetUp()
        {
            _source = new Set<int>();
            _subject = _source.Sort((i1, i2) => i1.CompareTo(i2));
        }

        [TestCase()]
        [TestCase(1)]
        [TestCase(1, 2)]
        [TestCase(1, 2, 3)]
        [TestCase(2, 1)]
        [TestCase(-1, 1)]
        [TestCase(1, -1)]
        [TestCase(1, 2, 3, 0)]
        public async Task AddItems(params int[] items)
        {
            foreach(var i in items)
                _source.SetItem(i);

            var snapshot = await _subject.SnapshotAsync();

            var itemsSorted = items.OrderBy(i => i).ToArray();

            for(var i = 0; i < itemsSorted.Length; i++)
                Assert.AreEqual(itemsSorted[i], snapshot[i]);           
        }

        [TestCase(new int[]{ 1 }, new int[]{ })]
        [TestCase(new int[]{ 1, 2 }, new int[]{ 2 })]
        [TestCase(new int[]{ 1, 2 }, new int[]{ 1 })]
        [TestCase(new int[]{ 1, 2, 3 }, new int[]{ 1, 2 })]
        [TestCase(new int[]{ 1, 2, 3 }, new int[]{ 1, 3 })]
        [TestCase(new int[]{ 2, 1 }, new int[]{ 1 })]
        [TestCase(new int[]{ 2, 1 }, new int[]{ 2 })]
        public async Task RemoveItems(int[] initial, int[] remove)
        {
            foreach(var i in initial)
                _source.SetItem(i);

            foreach(var i in remove)
                _source.DeleteItem(i);

            var snapshot = await _subject.SnapshotAsync();

            var itemsSorted = initial.Except(remove).OrderBy(i => i).ToArray();

            for(var i = 0; i < itemsSorted.Length; i++)
                Assert.AreEqual(itemsSorted[i], snapshot[i]);           
        }
    }
}

