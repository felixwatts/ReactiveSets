using System;
using System.Reactive.Linq;
using NUnit.Framework;
using ReactiveSets;
using Moq;
using System.Linq;

namespace ReactiveSets.Test
{
    [TestFixture]
    public class Aggregator
    {
        Set<string, int> _source;
        IValue<string> _subject;

        [SetUp]
        public void SetUp()
        {
            _source = new Set<string, int>();
            _subject = _source.Aggregate(xs => string.Join(",", xs.OrderBy(x => x).Select(x => x.ToString())));

        }

       [Test]
       public void InitiallyEmpty()
       {
           _subject.Subscribe();

           Assert.AreEqual("", _subject.Current);
       }
    }
}

