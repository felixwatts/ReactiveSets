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
    public class UnionOfSets
    {
        private Set<string, Set<string, string>> _input;
        private ReactiveSets.ISet<string, string> _subject;


        [SetUp]
        public void SetUp()
        {
            _input = new Set<string, Set<string, string>>();
            _subject = _input.Union();
        }
    }
}
