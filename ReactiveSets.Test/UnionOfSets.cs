using System;
using System.Linq;
using NUnit.Framework;
using Moq;
using ReactiveSets;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReactiveSets.Test
{
    [TestFixture]
    public class UnionOfSets
    {
        private Set<string, Set<string, string>> _input;
        private ISet<string, string> _subject;


        [SetUp]
        public void SetUp()
        {
            _input = new Set<string, Set<string, string>>();
            _subject = _input.Union();
        }

        [Test]
        public void WhenInputIsEmptyOutputIsEmpty()
        {
            _subject.Snapshot(content => Assert.IsEmpty(content));
        }

        [Test]
        public async Task SingleInputSet()
        {
            var set = new Set<string, string>();
            set.SetItem("A");
            set.SetItem("B");

            _input.SetItem("S1", set);

            var output = await _subject.SnapshotAsync();

            Assert.AreEqual(2, output.Count);
            Assert.AreEqual("A", output["A"]);
            Assert.AreEqual("B", output["B"]);
        }

        [Test]
        public async Task MultipleInputSets()
        {
            var set1 = new Set<string, string>();
            set1.SetItem("A");
            set1.SetItem("B");

            var set2 = new Set<string, string>();
            set2.SetItem("C");
            set2.SetItem("D");

            _input.SetItem("S1", set1);
            _input.SetItem("S2", set2);

            var output = await _subject.SnapshotAsync();

            Assert.AreEqual(4, output.Count);
            Assert.AreEqual("A", output["A"]);
            Assert.AreEqual("B", output["B"]);
            Assert.AreEqual("C", output["C"]);
            Assert.AreEqual("D", output["D"]);
        }

        [Test]
        public async Task RemoveAnInputSet()
        {
            var set1 = new Set<string, string>();
            set1.SetItem("A");
            set1.SetItem("B");

            var set2 = new Set<string, string>();
            set2.SetItem("C");
            set2.SetItem("D");

            _input.SetItem("S1", set1);
            _input.SetItem("S2", set2);

            _input.DeleteItem("S1");

            var output = await _subject.SnapshotAsync();

            Assert.AreEqual(2, output.Count);
            Assert.AreEqual("C", output["C"]);
            Assert.AreEqual("D", output["D"]);
        }

        [Test]
        public async Task AddAnItemToAnInputSet()
        {
            var set1 = new Set<string, string>();
            set1.SetItem("A");

            _input.SetItem("S1", set1);

            set1.SetItem("B");

            var output = await _subject.SnapshotAsync();

            Assert.AreEqual(2, output.Count);
            Assert.AreEqual("A", output["A"]);
            Assert.AreEqual("B", output["B"]);
        }

        [Test]
        public async Task RemoveAnItemFromAnInputSet()
        {
            var set1 = new Set<string, string>();
            set1.SetItem("A");
            set1.SetItem("B");

            _input.SetItem("S1", set1);

            set1.DeleteItem("B");

            var output = await _subject.SnapshotAsync();

            Assert.AreEqual(1, output.Count);
            Assert.AreEqual("A", output["A"]);
        }

        [Test]
        public async Task AddADuplicateItem()
        {
            var set1 = new Set<string, string>();
            set1.SetItem("A");
            set1.SetItem("B");

            var set2 = new Set<string, string>();

            _input.SetItem("S1", set1);
            _input.SetItem("S2", set2);

            set2.SetItem("A");

            var output = await _subject.SnapshotAsync();

            Assert.AreEqual(2, output.Count);
            Assert.AreEqual("A", output["A"]);
            Assert.AreEqual("B", output["B"]);
        }

        [Test]
        public async Task RemoveADuplicateItem()
        {
            var set1 = new Set<string, string>();
            set1.SetItem("A");
            set1.SetItem("B");

            var set2 = new Set<string, string>();
            set2.SetItem("A");

            _input.SetItem("S1", set1);
            _input.SetItem("S2", set2);

            set2.DeleteItem("A");

            var output = await _subject.SnapshotAsync();

            Assert.AreEqual(2, output.Count);
            Assert.AreEqual("A", output["A"]);
            Assert.AreEqual("B", output["B"]);
        }

        [Test]
        public async Task AddASetWithADuplicateItem()
        {
            var set1 = new Set<string, string>();
            set1.SetItem("A");
            set1.SetItem("B");

            var set2 = new Set<string, string>();
            set2.SetItem("B");
            set2.SetItem("C");

            _input.SetItem("S1", set1);
            _input.SetItem("S2", set2);

            var output = await _subject.SnapshotAsync();

            Assert.AreEqual(3, output.Count);
            Assert.AreEqual("A", output["A"]);
            Assert.AreEqual("B", output["B"]);
            Assert.AreEqual("C", output["C"]);
        }

        [Test]
        public async Task RemoveASetWithADuplicateItem()
        {
            var set1 = new Set<string, string>();
            set1.SetItem("A");
            set1.SetItem("B");

            var set2 = new Set<string, string>();
            set2.SetItem("B");
            set2.SetItem("C");

            _input.SetItem("S1", set1);
            _input.SetItem("S2", set2);

            _input.DeleteItem("S2");

            var output = await _subject.SnapshotAsync();

            Assert.AreEqual(2, output.Count);
            Assert.AreEqual("A", output["A"]);
            Assert.AreEqual("B", output["B"]);
        }
    }
}
