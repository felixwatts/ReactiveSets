using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReactiveSets
{
    public static class Extensions
    {
        public static IValue<T> ToValue<T>(this IObservable<T> source, T initialValue = default(T))
        {
            return new ValueFromObservable<T>(source, initialValue);
        }

        public static void SetItem<TId>(this Set<TId, TId> set, TId item)
        {
            set.SetItem(item, item);
        }

        public static void Snapshot<TId, TPayload>(this ISet<TId, TPayload> source, Action<IReadOnlyDictionary<TId, TPayload>> handleResult)
        {
            new Snapshotter<TId, TPayload>(source, handleResult);
        }

        public static Task<IReadOnlyDictionary<TId, TPayload>> SnapshotAsync<TId, TPayload>(this ISet<TId, TPayload> source)
        {
            return Task.Run(() =>
            {
                var t = new TaskCompletionSource<IReadOnlyDictionary<TId, TPayload>>();
                source.Snapshot(ss => t.TrySetResult(ss));
                return t.Task;
            });
        }

        public static ISet<TIdOut, TPayloadOut> Select<TIdIn, TPayloadIn, TIdOut, TPayloadOut>(
            this IObservable<Delta<TIdIn, TPayloadIn>> source, 
            Func<TIdIn, TIdOut> idMapping, 
            Func<TIdIn, TPayloadIn, TPayloadOut> payloadMapping,
            bool disposeOnDelete = true)
        {
            return new Mapper<TIdIn, TPayloadIn, TIdOut, TPayloadOut>(source, idMapping, payloadMapping, disposeOnDelete);
        }

        public static ISet<TIdIn, TPayloadOut> Select<TIdIn, TPayloadIn, TPayloadOut>(
            this IObservable<Delta<TIdIn, TPayloadIn>> source, 
            Func<TIdIn, TPayloadIn, TPayloadOut> payloadMapping,
            bool disposeOnDelete = true)
        {
            return new Mapper<TIdIn, TPayloadIn, TIdIn, TPayloadOut>(source, id => id, payloadMapping, disposeOnDelete);
        }

        public static ISet<TIdIn, TPayloadOut> Select<TIdIn, TPayloadIn, TPayloadOut>(
            this IObservable<Delta<TIdIn, TPayloadIn>> source, 
            Func<TPayloadIn, TPayloadOut> payloadMapping,
            bool disposeOnDelete = true)
        {
            return new Mapper<TIdIn, TPayloadIn, TIdIn, TPayloadOut>(source, id => id, (id, payload) => payloadMapping(payload), disposeOnDelete);
        }

        public static ISet<TId, TPayload> Union<TIdSet, TId,TPayload>(
            this IObservable<Delta<TIdSet, IObservable<Delta<TId, TPayload>>>> source)
        {
            return new UnionOfSets<TIdSet, TId, TPayload>(source);
        }

        public static ISet<TId, TPayload> Where<TId, TPayload>(
            this IObservable<Delta<TId, TPayload>> source,
            Func<TId, TPayload, bool> condition)
        {
            return new Subsetter<TId, TPayload>(source, condition);
        }

        public static ISet<TId, TPayload> Where<TId, TPayload>(
            this IObservable<Delta<TId, TPayload>> source,
            Predicate<TPayload> condition)
        {
            return new Subsetter<TId, TPayload>(source, (id, payload) => condition(payload));
        }

        public static ISet<TId, TPayload> WhereDynamic<TId, TPayload, TDynamic>(
            this IObservable<Delta<TId, TPayload>> source,
            Func<TPayload, IObservable<TDynamic>> payloadToObservable,
            Predicate<TDynamic> condition)
        {
            return new DynamicSubsetter<TId, TPayload, TDynamic>(source, payloadToObservable, condition);
        }

        public static IObservable<TPayloadOut> Aggregate<TIdIn, TPayloadIn, TPayloadOut>(
            this IObservable<Delta<TIdIn, TPayloadIn>> source,
            Func<IEnumerable<TPayloadIn>, TPayloadOut> aggregate)
        {
            return new Aggregator<TIdIn, TPayloadIn, TPayloadOut>(source, aggregate);
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

        public static void DisposeAll(this IEnumerable<IDisposable> disposables)
        {
            if(disposables == null) return;

            foreach(var disposable in disposables)
                disposable?.Dispose();
        }
    }
}