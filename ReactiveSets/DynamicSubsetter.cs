using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveSets
{
    internal class DynamicSubsetter<TId, TPayload, TDynamic> : DynamicToSet<TId, TPayload, TDynamic, TPayload>
    {
        private readonly Predicate<TDynamic> _condition;

        public DynamicSubsetter(
            IObservable<IDelta<TId, TPayload>> source, 
            Func<TPayload, IObservable<TDynamic>> payloadToObservable, 
            Predicate<TDynamic> condition) 
            : base(source, payloadToObservable)
        {
            _condition = condition;
        }

        protected override void OnDynamicNext(TId id, TDynamic next, TPayload payload)
        {
            var shouldInclude = _condition(next);            

            if(shouldInclude)
            {
                _content.SetItem(id, payload);
            }
            else
            {
                var isIncluded = _content.ContainsKey(id);
                
                if(isIncluded)
                {
                    _content.DeleteItem(id);
                }                
            }
        }
    }
} 