using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveSets
{
    internal class DynamicMapper<TId, TPayload, TDynamic> : DynamicToSet<TId, TPayload, TDynamic, TDynamic>
    {
        public DynamicMapper(
            IObservable<IDelta<TId, TPayload>> source,             
            Func<TId, TPayload, IObservable<TDynamic>> payloadToObservable) 
            : base(source, payloadToObservable)
        {
        }

        protected override void OnDynamicNext(TId id, TDynamic next, TPayload payload)
        {
            _content.SetItem(id, next);
        }
    }
} 