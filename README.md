# Introduction

Reactive Sets provides a declarative way to describe relationships between objects and have those relationships automatically maintained over time. 

Whereas LINQ allows you to describe a transformation of a collection and perform that trasformation once and Rx allows you to describe a transformation on a single value and have that transformation re-applied every time the value changes, Reactive Sets combines both these powers. You use a LINQ like syntax to describe a transformation of a collection and ReactiveSets ensures that the transformation is re-applied as the contents of the source collection change over time.

Practically speaking, Reactive Sets is mostly a set of extension methods on the type `IObservable<Delta<TId, TPayload>>`. `Delta<TId, TPayload>` is a struct describing a mutation to a set of items (add an item, remove an item, clear etc.). As in LINQ and Rx, the extension method usually returns a similar type so chaining is possible.

Here is a small example:

```csharp
public class Stock
{
    public string Name { get; }
    public IObservable<double> Price { get; }

    public Stock(string name)
    {
        Name = name;

        // stock price does a random walk starting at 100
        Price = Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Aggregate(100d, (a, b) => a + _random.NextDouble() - 0.5d);
    }

    private static Random _random = new Random();
}

public static void Test()
{
    var source = new Set<string>();                             // a set of stock names that can change over time

    source
        .Select(n => new Stock(n))                              // map from stock names to stock objects
        .WhereDynamic(s => s.Price, p => p > 101)               // filter for stocks who's price has moved a lot
        .Select(s => s.Name)                                    // map each stock to just its name
        .Aggregate(ns => string.Join(",", ns.OrderBy(n => n)))  // reduce the collection to a single string of stock names
        .Subscribe(str =>                                       // print the result each time it changes
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(str);
        });

    source.SetItem("GOOG");                                     // any manipulations to the source set will be taken
    source.SetItem("AAPL");                                     // into account
}
```