# Introduction

Reactive Sets provides a declarative way to describe relationships between objects and have those relationships automatically maintained over time. 

Whereas LINQ allows you to describe a transformation of a collection and perform that transformation once and Rx allows you to describe a transformation on a single value and have that transformation re-applied every time the value changes, Reactive Sets combines both these powers. You use a LINQ like syntax to describe a transformation of a collection and ReactiveSets ensures that the transformation is re-applied as the contents of the source collection change over time.

Practically speaking, Reactive Sets is mostly a set of extension methods on the type `IObservable<Delta<TId, TPayload>>`. `Delta<TId, TPayload>` is a struct describing a mutation to a set of items (add an item, remove an item, clear etc.). As in LINQ and Rx, the extension method usually returns a similar type so chaining is possible.

Here is a small example:

```csharp
// a set mapping stock ticker to current price
var stockPrices = new Set<string, double>();
// a set mapping stock ticker to number of stocks owned
var positions = new Set<string, int>();

positions                
    // join the two sets by stock ticker (the Id), multiplying the values
    // to give us a set of stock ticker to current value of the owned stocks
    .Join(stockPrices, (position, price) => position * price)
    // sum value over all stocks owned
    .Aggregate(vs => vs.Sum())
    // print the total value to the console each time it changes
    .Subscribe(Console.WriteLine);

// changes to stock prices and stocks owned
// automatically update the total value
stockPrices.SetItem("GOOG", 500);
positions.SetItem("GOOG", 10);
stockPrices.SetItem("AAPL", 1000);
positions.SetItem("AAPL", 10);
stockPrices.SetItem("AAPL", 1001);
positions.DeleteItem("AAPL");

// prints:
// 0
// 0
// 5000
// 15000
// 15010
// 5000
```