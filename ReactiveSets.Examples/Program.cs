using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;

namespace ReactiveSets.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            //Example1();
            //Example2();
            Example3();
        }

        private static void Example1()
        {
            // declare a set of integers
            var source = new Set<int>(); 

            source
                // reduce the set to the sum of its contents
                .Aggregate(ints => ints.Sum())
                // print the sum each time it changes
                .Subscribe(Console.WriteLine);

            // modifications to the set update
            // and print the sum accordingly        
            source.SetItem(2);
            source.SetItem(3);
            source.SetItem(37);
            source.DeleteItem(3);
        }

        private static void Example2()
        {
            // declare a set of ints keyed by string
            var source = new Set<string, int>();

            source
                // filter out even numbers
                .Where(i => i % 2 == 1)
                // reduce to a comma separated string
                .Aggregate(ints => string.Join(",", ints))
                // print the string each time it changes
                .Subscribe(Console.WriteLine);

            source.SetItem("A", 1);
            source.SetItem("B", 2);
            source.SetItem("C", 3);
            // keys are unique but values need not be
            source.SetItem("C", 1);
            source.DeleteItem("C");
        }

        private static void Example3()
        {
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
        }

        private static void Example4()
        {
            var source = new Set<string>();                                 // a set of stock names that can change over time

            source
                .Select(n => new Stock(n))                                  // map from stock names to stock objects
                .WhereDynamic(s => s.Price, p => p > 100)                 // filter for stocks who's price has moved a lot
                .SelectDynamic(s => s.Price.Select(p => $"{s.Name}:{p:f2}:{System.Threading.Thread.CurrentThread.ManagedThreadId}"))// map each stock to just its name
                .Aggregate(ns => string.Join(Environment.NewLine, ns.OrderBy(n => n)))      // reduce the collection to a single string of stock names
                .Subscribe(str =>                                           // print the result each time it changes
                {
                    Console.Clear();
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine(str);
                });

            source.SetItem("GOOG");                                     // any manipulations to the source set will be taken
            source.SetItem("AAPL");                                     // into account            

            Console.Read();
        }
    }

    public class Stock
    {
        public string Name { get; }
        public IValue<double> Price { get; }

        public Stock(string name)
        {
            Name = name;

            // stock price does a random walk starting at 100
            Price = _ticker
                .Scan(100d, (current, _) => current + SmallDelta())                  
                .ToValue();
        }

        private double SmallDelta()
        {
            return (_random.NextDouble() - 0.5d) * 0.1d;
        }

        private static Random _random = new Random();
        private static IObservable<long> _ticker = Observable.Interval(TimeSpan.FromSeconds(0.2)).ObserveOn(Scheduler.CurrentThread);
    }
}
