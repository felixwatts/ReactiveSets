using System;
using System.Collections.Generic;

namespace ReactiveSets
{
    internal static class Extensions
    {
        public static ISet<TIdOut, TPayloadOut> Select<TIdIn, TPayloadIn, TIdOut, TPayloadOut>(
            this ISet<TIdIn, TPayloadIn> source, 
            Func<TIdIn, TIdOut> idMapping, 
            Func<TIdIn, TPayloadIn, TPayloadOut> payloadMapping,
            bool disposeOnDelete = true)
        {
            return new Mapper<TIdIn, TPayloadIn, TIdOut, TPayloadOut>(source, idMapping, payloadMapping, disposeOnDelete);
        }

        public static TValue FindOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> valueFactory)
        {
            TValue val;
            if(!dict.TryGetValue(key, out val))
            {
                val = valueFactory();
                dict.Add(key, val);
            }

            return val;
        }
    }
}