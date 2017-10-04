using System;
using System.Collections.Generic;

namespace ReactiveSets
{
    internal class Joiner<TId, TPayloadLeft, TPayloadRight, TPayloadOut>
        : TwoSetsToSet<TId, TPayloadLeft, TId, TPayloadRight, TId, TPayloadOut>
    {
        private class Pair
        {
            public TPayloadLeft Left;
            public bool IsLeftPresent;
            public TPayloadRight Right;
            public bool IsRightPresent;
        }

        private readonly Dictionary<TId, Pair> _data;
        private readonly Func<TPayloadLeft, TPayloadRight, TPayloadOut> _join;

        public Joiner(
            ISet<TId, TPayloadLeft> left, 
            ISet<TId, TPayloadRight> right,
            Func<TPayloadLeft, TPayloadRight, TPayloadOut> join,
            bool disposeItemsOnDelete) 
            : base(left, right, disposeItemsOnDelete)
        {
            _data = new Dictionary<TId, Pair>();
            _join = join;
        }

        protected override void Reset()
        {
            _data.Clear();
            _content.Clear();
        }

        internal override void OnClearLeft()
        {
            foreach(var kvp in _data)
            {
                var id = kvp.Key;
                var pair = kvp.Value;
                pair.IsLeftPresent = false;
                pair.Left = default(TPayloadLeft);
            }
            _content.Clear();
        }

        internal override void OnClearRight()
        {
            foreach(var kvp in _data)
            {
                var id = kvp.Key;
                var pair = kvp.Value;
                pair.IsRightPresent = false;
                pair.Right = default(TPayloadRight);
            }
            _content.Clear();
        }

        internal override void OnDeleteItemLeft(TId id)
        {
            var pair = _data.FindOrAdd(id, () => new Pair());
            pair.IsLeftPresent = false;
            pair.Left = default(TPayloadLeft);
            Refresh(id, pair);
        }

        internal override void OnDeleteItemRight(TId id)
        {
            var pair = _data.FindOrAdd(id, () => new Pair());
            pair.IsRightPresent = false;
            pair.Right = default(TPayloadRight);
            Refresh(id, pair);
        }

        internal override void OnSetItemLeft(TId id, TPayloadLeft payload)
        {
            var pair = _data.FindOrAdd(id, () => new Pair());
            pair.IsLeftPresent = true;
            pair.Left = payload;
            Refresh(id, pair);
        }

        internal override void OnSetItemRight(TId id, TPayloadRight payload)
        {
            var pair = _data.FindOrAdd(id, () => new Pair());
            pair.IsRightPresent = true;
            pair.Right = payload;
            Refresh(id, pair);
        }

        private void Refresh(TId id, Pair pair)
        {
            var isIncluded = _content.ContainsKey(id);

            var shouldInclude = pair.IsLeftPresent && pair.IsRightPresent;

            if(shouldInclude)
            {
                var newItem = _join(pair.Left, pair.Right);
                _content.SetItem(id, newItem);
            }
            else if(isIncluded)
            {
                _content.DeleteItem(id);
            }
        }
    }
}