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

        // Just used to make C# collection initializer syntax work
        public static void Add<TId, TPayload>(this Set<TId, TPayload> set, TId id, TPayload payload)
        {
            set.SetItem(id, payload);
        }

        // Just used to make C# collection initializer syntax work
        public static void Add<TId>(this Set<TId, TId> set, TId item)
        {
            set.SetItem(item, item);
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
            this IObservable<IDelta<TIdIn, TPayloadIn>> source, 
            Func<TIdIn, TIdOut> idMapping, 
            Func<TIdIn, TPayloadIn, TPayloadOut> payloadMapping,
            bool disposeOnDelete = true)
        {
            return new Mapper<TIdIn, TPayloadIn, TIdOut, TPayloadOut>(source, idMapping, payloadMapping, disposeOnDelete);
        }

        public static ISet<TIdIn, TPayloadOut> Select<TIdIn, TPayloadIn, TPayloadOut>(
            this IObservable<IDelta<TIdIn, TPayloadIn>> source, 
            Func<TIdIn, TPayloadIn, TPayloadOut> payloadMapping,
            bool disposeOnDelete = true)
        {
            return new Mapper<TIdIn, TPayloadIn, TIdIn, TPayloadOut>(source, id => id, payloadMapping, disposeOnDelete);
        }

        public static ISet<TIdIn, TPayloadOut> Select<TIdIn, TPayloadIn, TPayloadOut>(
            this IObservable<IDelta<TIdIn, TPayloadIn>> source, 
            Func<TPayloadIn, TPayloadOut> payloadMapping,
            bool disposeOnDelete = true)
        {
            return new Mapper<TIdIn, TPayloadIn, TIdIn, TPayloadOut>(source, id => id, (id, payload) => payloadMapping(payload), disposeOnDelete);
        }

        public static ISet<TIdIn, TPayloadOut> SelectDynamic<TIdIn, TPayloadIn, TPayloadOut>(
            this IObservable<IDelta<TIdIn, TPayloadIn>> source,
            Func<TPayloadIn, IObservable<TPayloadOut>> payloadToObservable)
        {
            return new DynamicMapper<TIdIn, TPayloadIn, TPayloadOut>(source, payloadToObservable);
        }

        public static ISet<TId, TPayload> Union<TIdSet, TId,TPayload>(
            this IObservable<IDelta<TIdSet, IObservable<IDelta<TId, TPayload>>>> source)
        {
            return new UnionOfSets<TIdSet, TId, TPayload>(source);
        }

        public static ISet<TId, TPayload> Union<TId, TPayload>(
            this IEnumerable<IObservable<IDelta<TId, TPayload>>> sets)
        {
            var container = new Set<int, IObservable<IDelta<TId, TPayload>>>();
            int n = 0;
            foreach(var set in sets)
            {
                container.SetItem(n , set);
                n++;
            }
            return container.Union();
        }

        public static ISet<TId, TPayload> Intersection<TIdSet, TId,TPayload>(
            this IObservable<IDelta<TIdSet, IObservable<IDelta<TId, TPayload>>>> source)
        {
            return new IntersectionOfSets<TIdSet, TId, TPayload>(source);
        }

        public static ISet<TId, TPayload> Intersection<TId, TPayload>(
            this IEnumerable<IObservable<IDelta<TId, TPayload>>> sets)
        {
            var container = new Set<int, IObservable<IDelta<TId, TPayload>>>();
            int n = 0;
            foreach(var set in sets)
            {
                container.SetItem(n , set);
                n++;
            }
            return container.Intersection();
        }

        public static ISet<TId, TPayload> Difference<TIdSet, TId,TPayload>(
            this IObservable<IDelta<TIdSet, IObservable<IDelta<TId, TPayload>>>> source)
        {
            return new DifferenceOfSets<TIdSet, TId, TPayload>(source);
        }

        public static ISet<TId, TPayload> Difference<TId, TPayload>(
            this IEnumerable<IObservable<IDelta<TId, TPayload>>> sets)
        {
            var container = new Set<int, IObservable<IDelta<TId, TPayload>>>();
            int n = 0;
            foreach(var set in sets)
            {
                container.SetItem(n , set);
                n++;
            }
            return container.Difference();
        }

        public static ISet<TId, TPayload> Where<TId, TPayload>(
            this IObservable<IDelta<TId, TPayload>> source,
            Func<TId, TPayload, bool> condition)
        {
            return new Subsetter<TId, TPayload>(source, condition);
        }

        public static ISet<TId, TPayload> Where<TId, TPayload>(
            this IObservable<IDelta<TId, TPayload>> source,
            Predicate<TPayload> condition)
        {
            return new Subsetter<TId, TPayload>(source, (id, payload) => condition(payload));
        }

        public static ISet<TId, TPayload> WhereDynamic<TId, TPayload, TDynamic>(
            this IObservable<IDelta<TId, TPayload>> source,
            Func<TPayload, IObservable<TDynamic>> payloadToObservable,
            Predicate<TDynamic> condition)
        {
            return new DynamicSubsetter<TId, TPayload, TDynamic>(source, payloadToObservable, condition);
        }

        public static IValue<TPayloadOut> Aggregate<TIdIn, TPayloadIn, TPayloadOut>(
            this IObservable<IDelta<TIdIn, TPayloadIn>> source,
            Func<IEnumerable<TPayloadIn>, TPayloadOut> aggregate,
            TPayloadOut initialValue = default(TPayloadOut))
        {
            return new Aggregator<TIdIn, TPayloadIn, TPayloadOut>(source, aggregate, initialValue);
        }  

        public static ISet<TId, TPayloadOut> Join<TId, TPayloadLeft, TPayloadRight, TPayloadOut>(
            this ISet<TId, TPayloadLeft> left,
            ISet<TId, TPayloadRight> right,
            Func<TPayloadLeft, TPayloadRight, TPayloadOut> join,
            bool disposeItemsOnDelete = false)
        {
            return new Joiner<TId, TPayloadLeft, TPayloadRight, TPayloadOut>(left, right, join, disposeItemsOnDelete);
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